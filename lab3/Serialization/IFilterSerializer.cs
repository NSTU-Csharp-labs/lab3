using System.Collections.Generic;
using lab3.Controls.GL;

namespace lab3.Serialization;

public interface IFilterSerializer
{
    public IEnumerable<Filter> LoadFilters();
}