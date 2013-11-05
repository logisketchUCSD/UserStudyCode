using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RecognitionInterfaces;
using Sketch;
using Featurefy;
using ContextDomain;
using Data;

namespace Refiner
{

    #region Helper Classes - Search Methods

    interface ISearchMethod
    {

        /// <summary>
        /// Choose the action to take from a list of available actions.
        /// </summary>
        /// <param name="availableActions">the list of available actions</param>
        /// <param name="sketch">the sketch, which will be unmodified at the end of this function</param>
        /// <returns>the action that should be taken, or null if the search should terminate</returns>
        ISketchModification chooseAction(
            List<ISketchModification> availableActions, 
            ContextDomain.ContextDomain domain, 
            Sketch.Sketch sketch);

    }

    class HillClimbSearch : ISearchMethod
    {

        public ISketchModification chooseAction(List<ISketchModification> availableActions, ContextDomain.ContextDomain domain, Sketch.Sketch sketch)
        {

            ISketchModification result = null;

            double max = 0;

            foreach (ISketchModification action in availableActions)
            {
                double change = action.benefit();
                if (change > max)
                {
                    result = action;
                    max = change;
                }
            }

            return result;

        }

    }

    #endregion

    /// <summary>
    /// The search refiner works on the following premise:
    /// 
    ///     The user drew a sketch that, when correctly recognized,
    ///     is a completely valid circuit.
    /// 
    /// The goal of this refiner, then, is to search for that interpretation.
    /// It requires that the other parts of the recognition process are capable
    /// of leading us in the right direction, but no more than that.
    /// 
    /// This works by using the CorrectnessScore method of a ContextDomain
    /// object to assess the quality of recognition. 
    /// 
    /// The SearchRefiner operates independently of any particular domain.
    /// </summary>
    public class SearchRefiner : IRecognitionStep
    {

        private ContextDomain.ContextDomain _domain;
        private ISearchMethod _searchMethod;

        public SearchRefiner(ContextDomain.ContextDomain domain)
        {
            _domain = domain;
            _searchMethod = new HillClimbSearch();
        }

        public void process(FeatureSketch featureSketch)
        {
            Sketch.Sketch sketch = featureSketch.Sketch;
            ISketchModification actionToTake;
            const int MAX_ITERATIONS = 50;

            int i = 0;
            while ((actionToTake = _searchMethod.chooseAction(_domain.SketchModifications(sketch), _domain, sketch)) != null)
            {
                i++;
                if (i > MAX_ITERATIONS)
                    return;
                actionToTake.perform();
            }
        }

    }

}
