using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.AdminTrees.Models;
using OrchardCore.AdminTrees.Services;
using OrchardCore.AdminTrees.AdminNodes;
using OrchardCore.Navigation;
using System.Threading.Tasks;

namespace OrchardCore.AdminTrees.AdminNodes
{
    public class LinkAdminNodeNavigationBuilder : IAdminNodeNavigationBuilder
    {
        private readonly ILogger<LinkAdminNodeNavigationBuilder> _logger;

        public LinkAdminNodeNavigationBuilder(ILogger<LinkAdminNodeNavigationBuilder> logger)
        {
            _logger = logger;
        }

        public string Name => typeof(LinkAdminNode).Name;


        public Task BuildNavigationAsync(MenuItem menuItem, NavigationBuilder builder, IEnumerable<IAdminNodeNavigationBuilder> treeNodeBuilders)
        {
            var node = menuItem as LinkAdminNode;

            if (node == null || String.IsNullOrEmpty(node.LinkText) || !node.Enabled)
            {
                return Task.CompletedTask;
            }

            builder.Add(new LocalizedString(node.LinkText, node.LinkText), async itemBuilder => {

                // Add the actual link
                itemBuilder.Url(node.LinkUrl);
                itemBuilder.Priority(node.Priority);

                // Add adminNode's IconClass property values to menuItem.Classes. 
                // Add them with a prefix so that later the shape template can extract them to use them on a <i> tag.
                node.IconClass?.Split(' ').ToList().ForEach(c => itemBuilder.AddClass("icon-class-" + c));

                // Let children build themselves inside this MenuItem
                // todo: this logic can be shared by all TreeNodeNavigationBuilders
                foreach (var childTreeNode in menuItem.Items)
                {
                    try
                    {
                        var treeBuilder = treeNodeBuilders.Where(x => x.Name == childTreeNode.GetType().Name).FirstOrDefault();
                        await treeBuilder.BuildNavigationAsync(childTreeNode, itemBuilder, treeNodeBuilders);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "An exception occurred while building the '{MenuItem}' child Menu Item.", childTreeNode.GetType().Name);
                    }
                }
            });

            return Task.CompletedTask;
        }

        // Add adminNode's IconClass property values to menuItem.Classes. 
        // Add them with a prefix so that later the shape template can extract them to use them on a <i> tag.
        private void AddIconPickerClassToLink(string iconClass, NavigationItemBuilder itemBuilder)
        {
            if (String.IsNullOrEmpty(iconClass))
            {
                return;
            }
            
            foreach (var c in iconClass.Split(' ' ))
            {
                itemBuilder.AddClass("icon-class-" + c);
            }
        }
    }
}
