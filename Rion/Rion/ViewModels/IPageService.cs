using System.Threading.Tasks;
using Xamarin.Forms;

namespace Rion.ViewModels
{
    public interface IPageService
    {
        Task PushAsync(Page page);
    }
}