using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

using ScissorHands.Core.Models;
using ScissorHands.Theme;

namespace ScissorHands.Web;

public partial class MainLayout : MainLayoutBase
{
    private RenderFragment RenderNavigation(IReadOnlyList<NavigationNode> nodes) => builder =>
    {
        foreach (var item in nodes)
        {
            var hasChildren = item.Children.Count > 0;
            var submenuId = $"navigation-submenu-{item.Path}";

            builder.OpenElement(0, "li");
            builder.AddAttribute(1, "class", "navigation-item");
            builder.OpenElement(2, "div");
            builder.AddAttribute(3, "class", "navigation-link");

            if (item.Url is not null)
            {
                builder.OpenElement(4, "a");
                builder.AddAttribute(5, "href", item.Url);
                builder.AddContent(6, item.Title);
                builder.CloseElement();
            }
            else
            {
                builder.OpenElement(7, "span");
                builder.AddAttribute(8, "class", "navigation-label");
                builder.AddContent(9, item.Title);
                builder.CloseElement();
            }

            if (hasChildren)
            {
                builder.OpenElement(10, "button");
                builder.AddAttribute(11, "class", "navigation-toggle");
                builder.AddAttribute(12, "type", "button");
                builder.AddAttribute(13, "aria-expanded", "false");
                builder.AddAttribute(14, "aria-controls", submenuId);
                builder.AddAttribute(15, "aria-label", $"Toggle {item.Title} pages");
                builder.AddAttribute(16, "hidden", true);
                builder.OpenElement(17, "svg");
                builder.AddAttribute(18, "viewBox", "0 0 16 16");
                builder.AddAttribute(19, "aria-hidden", "true");
                builder.OpenElement(20, "path");
                builder.AddAttribute(21, "d", "m4 6 4 4 4-4");
                builder.CloseElement();
                builder.CloseElement();
                builder.CloseElement();
            }

            builder.CloseElement();

            if (hasChildren)
            {
                builder.OpenElement(22, "ul");
                builder.AddAttribute(23, "class", "navigation-children");
                builder.AddAttribute(24, "id", submenuId);
                builder.AddContent(25, RenderNavigation(item.Children));
                builder.CloseElement();
            }

            builder.CloseElement();
        }
    };
}
