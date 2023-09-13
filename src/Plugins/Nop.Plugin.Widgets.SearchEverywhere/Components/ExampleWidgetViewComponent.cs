using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

public class ExampleWidgetViewComponent : NopViewComponent
{
    public IViewComponentResult Invoke(string widgetZone, object additionalData)
    {
        return Content("Hello World");
    }
}