using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.Models.ViewModels
{
    public class UserVM
    {
        public ApplicationUser User { get; set; }

        public IEnumerable<SelectListItem> Roles { get; set; }
        public IEnumerable<SelectListItem> Companies { get; set; }
    }
}
