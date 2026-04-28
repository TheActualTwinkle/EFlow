namespace EFlow.Notifications.Templates.Rendering.Interfaces;

public interface ITemplateRenderer
{
    Task<string> RenderAsync<TModel>(string viewPath, TModel model, CancellationToken cancellationToken = new());
}
