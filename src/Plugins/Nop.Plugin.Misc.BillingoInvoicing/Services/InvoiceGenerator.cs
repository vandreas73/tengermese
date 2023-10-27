using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Spreadsheet;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Events;
using Nop.Services.Orders;

namespace Nop.Plugin.Misc.BillingoInvoicing.Services
{
    public class InvoiceGenerator : IConsumer<OrderPaidEvent>
    {
        private HttpClient _httpClient;
        private BillingoClientService _billingoService;
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly IAddressService _addressService;
        private readonly ICountryService _countryService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;

        public InvoiceGenerator(HttpClient httpClient,
            BillingoClientService billingoClientService,
            IOrderService orderService,
            IProductService productService,
            IAddressService addressService,
            ICountryService countryService,
            IStoreContext storeContext,
            ISettingService settingService)
        {
            _httpClient = httpClient;
            _billingoService = billingoClientService;
            _orderService = orderService;
            _productService = productService;
            _addressService = addressService;
            _countryService = countryService;
            _storeContext = storeContext;
            _settingService = settingService;
        }

        public async Task HandleEventAsync(OrderPaidEvent eventMessage)
        {
            Console.WriteLine("InvoiceGenerator HandleEventAsync");
            if (eventMessage?.Order != null)
            {
                Console.WriteLine("InvoiceGenerator HandleEventAsync order not null");

                var orderItems = await _orderService.GetOrderItemsAsync(eventMessage.Order.Id);
                var currentAddress = await _addressService.GetAddressByIdAsync(eventMessage.Order.BillingAddressId);
                var currentCountry = (await _countryService.GetCountryByAddressAsync(currentAddress));
                var partnersWithName = await _billingoService.ListPartnerAsync(1, 100, $"{currentAddress.LastName} {currentAddress.FirstName}");
                Partner currentPartner = partnersWithName.Data.Where(partner => partner.Iban == eventMessage.Order.CardNumber
                        && partner.Address.Country_code.ToString() == currentCountry.TwoLetterIsoCode
                        && partner.Address.City == currentAddress.City
                        && partner.Address.Post_code == currentAddress.ZipPostalCode
                        && partner.Address.Address1 == currentAddress.Address1).FirstOrDefault();

                if (currentPartner == null)
                {
                    var partner = new Partner()
                    {
                        Name = $"{currentAddress.LastName} {currentAddress.FirstName}",
                        Address = new Address
                        {
                            Address1 = currentAddress.Address1,
                            City = currentAddress.City,
                            Country_code = currentCountry.TwoLetterIsoCode,
                            Post_code = currentAddress.ZipPostalCode
                        },
                        Emails = new List<string>() { currentAddress.Email }
                    };
                    currentPartner = await _billingoService.CreatePartnerAsync(partner);
                }

                var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
                var settings = await _settingService.LoadSettingAsync<BillingoInvoicingSettings>(storeId);

                var document = new DocumentInsert()
                {
                    Partner_id = currentPartner.Id, //1802491707,
                    Block_id = settings.BlockId, //217127,
                    Bank_account_id = 179109,
                    Type = "invoice",
                    Fulfillment_date = DateTime.Now,
                    Due_date = DateTime.Now,
                    Payment_method = "paypal",
                    Language = "hu",
                    Currency = "HUF",
                    Conversion_rate = 1,
                    Electronic = false,
                    Paid = true,
                    Items = new List<DocumentProductData>(),
                    Settings = new DocumentSettings()
                    {
                        Mediated_service = false,
                        Without_financial_fulfillment = false,
                        Round = "one",
                        No_send_onlineszamla_by_user = true,
                        Order_number = eventMessage.Order.Id.ToString(),
                        Place_id = 0,
                        Instant_payment = true,
                        Selected_type = "invoice"
                    },
                };
                foreach (var orderItem in orderItems)
                {
                    var product = await _productService.GetProductByIdAsync(orderItem.ProductId);
                    var productData = new DocumentProductData()
                    {
                        Name = product.Name,
                        Unit_price = (float)orderItem.UnitPriceInclTax,
                        Unit_price_type = "gross",
                        Quantity = orderItem.Quantity,
                        Unit = "db",
                        Vat = "27%",
                        Entitlement = "AAM"
                    };
                    document.Items.Add(productData);
                }
                var invoice = await _billingoService.CreateDocumentAsync(document);

                await _billingoService.SendDocumentAsync(invoice.Id, new SendDocument
                {
                    Emails = new List<string>() { currentAddress.Email },
                });
            }
        }
    }
}
