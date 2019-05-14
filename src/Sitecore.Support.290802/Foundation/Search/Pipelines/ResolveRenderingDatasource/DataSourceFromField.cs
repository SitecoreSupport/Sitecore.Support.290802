namespace Sitecore.Support.Foundation.Search.Pipelines.ResolveRenderingDatasource
{
  using System;
  using Sitecore.Data.Fields;
  using Sitecore.Data.Items;
  using Sitecore.DependencyInjection;
  using Sitecore.Diagnostics;
  using Sitecore.Pipelines.ResolveRenderingDatasource;
  using Sitecore.XA.Foundation.Abstractions;
  using Sitecore.XA.Foundation.LocalDatasources.Pipelines.ResolveRenderingDatasource;
  using Sitecore.XA.Foundation.SitecoreExtensions.Extensions;
  using Microsoft.Extensions.DependencyInjection;
  using Sitecore.XA.Foundation.Multisite.Extensions;

  public class DataSourceFromField:DatasourceFromField
  {
    public new void Process(ResolveRenderingDatasourceArgs args)
    {
      Assert.IsNotNull(args, "args");

      if (args.Datasource.StartsWith(Sitecore.Support.Foundation.LocalDatasources.Constants.FieldPrefix, StringComparison.InvariantCultureIgnoreCase))
      {
        Item contextItem = args.GetContextItem();
        if (contextItem != null)
        {
          if (!ServiceLocator.ServiceProvider.GetService<IContext>().Site.IsSxaSite())
          {
            return;
          }

          string fieldName = args.Datasource.Substring(Sitecore.Support.Foundation.LocalDatasources.Constants.FieldPrefix.Length);
          LinkField dataSourceLink = contextItem.Fields[fieldName];
          if (dataSourceLink != null && dataSourceLink.TargetItem != null)
          {
            args.Datasource = dataSourceLink.TargetItem.ID.ToString();
            return;
          }
          if (!string.IsNullOrEmpty(contextItem[fieldName]))
          {
            args.Datasource = contextItem[fieldName];
            return;
          }
        }

        args.Datasource = string.Empty;
      }
    }
  }
}