using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ContextDomain
{

    /// <summary>
    /// The glorious Search Refiner needs to know what modifications are possible in
    /// a given domain. This interface encapsulates that.
    /// </summary>
    public interface ISketchModification
    {

        /// <summary>
        /// Perform this modification.
        /// </summary>
        /// <param name="sketch"></param>
        void perform();

        /// <summary>
        /// Determine how good it would be to perform this action. Higher
        /// numbers correspond (roughly) to better actions.
        /// </summary>
        /// <returns></returns>
        double benefit();

    }

}
