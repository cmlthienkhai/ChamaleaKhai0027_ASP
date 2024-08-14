using System.Collections.Generic;

namespace ChamaleaKhai0027.Context
{
    public class CombinedViewModel
    {
        public IEnumerable<Category> Categories { get; set; }
        public IEnumerable<Product> Products { get; set; }
    }
}
