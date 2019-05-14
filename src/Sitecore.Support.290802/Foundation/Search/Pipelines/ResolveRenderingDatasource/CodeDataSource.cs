namespace Sitecore.Support.Foundation.Search.Pipelines.ResolveRenderingDatasource
{
  using Sitecore.Data.Items;
  using Sitecore.Diagnostics;
  using Sitecore.Pipelines.ResolveRenderingDatasource;
  using Sitecore.Text;
  using Sitecore.XA.Foundation.LocalDatasources.Pipelines.ResolveRenderingDatasource;
  using Sitecore.XA.Foundation.SitecoreExtensions.Extensions;
  using System;
  using System.Linq;

  public class CodeDataSource : CodeDatasource
  {
    public new void Process(ResolveRenderingDatasourceArgs args)
    {
      Assert.IsNotNull(args, "args");
      if (args.Datasource.StartsWith(Sitecore.Support.Foundation.LocalDatasources.Constants.CodePrefix,
        StringComparison.OrdinalIgnoreCase))
      {
        string typeName = args.Datasource.Replace(Sitecore.Support.Foundation.LocalDatasources.Constants.CodePrefix,
          string.Empty);
        Item currentItem = args.GetContextItem();
        if (currentItem != null)
        {
          var items = CodeDatasourceService.GetDatasoureces(currentItem, typeName);
          ListString itemIds = new ListString(items.Select(i => i.ID.ToString()).ToList());
          args.Datasource = itemIds.ToString();
          return;
        }

        args.Datasource = string.Empty;
      }
    }
  }
}