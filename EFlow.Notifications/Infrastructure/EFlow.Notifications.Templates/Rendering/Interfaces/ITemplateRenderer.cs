namespace EFlow.Notifications.Templates.Rendering.Interfaces;

internal interface ITemplateRenderer
{
    Task<string> RenderAsync<TModel>(string viewPath, TModel model, CancellationToken cancellationToken = new());
}
