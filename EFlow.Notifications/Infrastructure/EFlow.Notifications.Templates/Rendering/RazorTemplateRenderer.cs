using System.Globalization;
using EFlow.Notifications.Templates.Rendering.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;

namespace EFlow.Notifications.Templates.Rendering;

public sealed class RazorTemplateRenderer(
    IRazorViewEngine razorViewEngine,
    ITempDataProvider tempDataProvider,
    IServiceProvider serviceProvider) : ITemplateRenderer
{
    public async Task<string> RenderAsync<TModel>(string viewPath, TModel model, CancellationToken cancellationToken = new())
    {
        var actionContext = CreateActionContext();
        var viewResult = razorViewEngine.GetView(null, viewPath, true);

        if (!viewResult.Success)
            viewResult = razorViewEngine.FindView(actionContext, viewPath, true);

        if (!viewResult.Success)
            throw new InvalidOperationException($"Razor view '{viewPath}' was not found.");

        await using var writer = new StringWriter(CultureInfo.InvariantCulture);

        var viewDictionary = new ViewDataDictionary<TModel>(
            new EmptyModelMetadataProvider(),
            new ModelStateDictionary())
        {
            Model = model
        };

        var viewContext = new ViewContext(
            actionContext,
            viewResult.View,
            viewDictionary,
            new TempDataDictionary(actionContext.HttpContext, tempDataProvider),
            writer,
            new HtmlHelperOptions());

        cancellationToken.ThrowIfCancellationRequested();
        await viewResult.View.RenderAsync(viewContext);

        return writer.ToString();
    }

    private ActionContext CreateActionContext()
    {
        var httpContext = new DefaultHttpContext
        {
            RequestServices = serviceProvider
        };

        return new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
    }
}
