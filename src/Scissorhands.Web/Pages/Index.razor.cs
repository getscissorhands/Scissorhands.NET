using Microsoft.AspNetCore.Components;

namespace Scissorhands.Web.Pages
{
    public partial class Index : ComponentBase
    {
        [Inject]
        protected HttpClient Http { get; set; }
    }
}