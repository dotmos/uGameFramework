using System;
using System.Collections.Generic;
using System.Linq;
using ModestTree;
using ModestTree.Util;

namespace Zenject
{
    // See comment in IGuiRenderable.cs for usage
    public class GuiRenderableManager
    {
        List<RenderableInfo> _renderables;

        public GuiRenderableManager(
            [Inject(Optional = true, Source = InjectSources.Local)]
            List<IGuiRenderable> renderables,
            [Inject(Optional = true, Source = InjectSources.Local)]
            List<ModestTree.Util.ValuePair<Type, int>> priorities)
        {
            _renderables = new List<RenderableInfo>();

            foreach (IGuiRenderable renderable in renderables)
            {
                // Note that we use zero for unspecified priority
                // This is nice because you can use negative or positive for before/after unspecified
                List<int> matches = priorities
                    .Where(x => renderable.GetType().DerivesFromOrEqual(x.First))
                    .Select(x => x.Second).ToList();

                int priority = matches.IsEmpty() ? 0 : matches.Distinct().Single();

                _renderables.Add(
                    new RenderableInfo(renderable, priority));
            }

            _renderables = _renderables.OrderBy(x => x.Priority).ToList();

#if UNITY_EDITOR
            foreach (IGuiRenderable renderable in _renderables.Select(x => x.Renderable).GetDuplicates())
            {
                Assert.That(false, "Found duplicate IGuiRenderable with type '{0}'".Fmt(renderable.GetType()));
            }
#endif
        }

        public void OnGui()
        {
            foreach (RenderableInfo renderable in _renderables)
            {
                try
                {
#if UNITY_EDITOR
                    using (ProfileBlock.Start("{0}.GuiRender()", renderable.Renderable.GetType()))
#endif
                    {
                        renderable.Renderable.GuiRender();
                    }
                }
                catch (Exception e)
                {
                    throw Assert.CreateException(
                        e, "Error occurred while calling {0}.GuiRender", renderable.Renderable.GetType());
                }
            }
        }

        class RenderableInfo
        {
            public IGuiRenderable Renderable;
            public int Priority;

            public RenderableInfo(IGuiRenderable renderable, int priority)
            {
                Renderable = renderable;
                Priority = priority;
            }
        }
    }
}
