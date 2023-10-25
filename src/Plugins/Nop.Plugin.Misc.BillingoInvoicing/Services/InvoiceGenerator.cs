using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Nop.Core.Domain.Orders;
using Nop.Services.Events;

namespace Nop.Plugin.Misc.BillingoInvoicing.Services
{
    public class InvoiceGenerator : IConsumer<OrderPaidEvent>
    {
        private HttpClient _httpClient;
        private BillingoClientService _billingoService;

        public InvoiceGenerator(HttpClient httpClient,
            BillingoClientService billingoClientService)
        {
            _httpClient = httpClient;
            _billingoService = billingoClientService;
        }

        public async Task HandleEventAsync(OrderPaidEvent eventMessage)
        {
            Console.WriteLine("InvoiceGenerator HandleEventAsync");
            if (eventMessage?.Order != null)
            {
                Console.WriteLine("InvoiceGenerator HandleEventAsync order not null");
                var document = new DocumentInsert()
                {
                    Partner_id = 1802491707,
                    Block_id = 217127,
                    Bank_account_id = 179109,
                    Type = DocumentInsertType.Invoice,
                    Fulfillment_date = DateTime.Now,
                    Due_date = DateTime.Now,
                    Payment_method = PaymentMethod.Paypal,
                    Language = DocumentLanguage.Hu,
                    Currency = Currency.HUF,
                    Conversion_rate = 1,
                    Electronic = false,
                    Paid = true,
                    Items = new List<DocumentProductData>()
                    {
                        new DocumentProductData()
                        {
                            Name = "item name",
                            Unit_price = (float)eventMessage.Order.OrderTotal,
                            Unit_price_type = UnitPriceType.Gross,
                            Quantity = 1,
                            Unit = "db",
                            Vat = "27%",
                            Entitlement = Entitlement.AAM
                        }
                    },
                    Settings = new DocumentSettings()
                    {
                        Mediated_service = false,
                        Without_financial_fulfillment = false,
                        Round = Round.One,
                        No_send_onlineszamla_by_user = true,
                        Order_number = eventMessage.Order.OrderGuid.ToString(),
                        Place_id = 0,
                        Instant_payment = true,
                        Selected_type = DocumentType.Invoice
                    },
                };
                await _billingoService.CreateDocumentAsync(document);
            }
        }
    }
}
