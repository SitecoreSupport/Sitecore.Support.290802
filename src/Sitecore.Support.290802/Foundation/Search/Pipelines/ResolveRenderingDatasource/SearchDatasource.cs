namespace Sitecore.Support.Foundation.Search.Pipelines.ResolveRenderingDatasource
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Web;
  using Sitecore.ContentSearch;
  using Sitecore.ContentSearch.Utilities;
  using Sitecore.Data.Items;
  using Sitecore.Diagnostics;
  using Sitecore.Pipelines.ResolveRenderingDatasource;
  using Sitecore.Text;
  using Sitecore.XA.Foundation.Search;
  using Sitecore.XA.Foundation.Search.Models;
  using Sitecore.XA.Foundation.SitecoreExtensions.Extensions;
  public class SearchDatasource: Sitecore.XA.Foundation.Search.Pipelines.ResolveRenderingDatasource.SearchDatasource
  {
    public new void Process(ResolveRenderingDatasourceArgs args)
    {
      Assert.IsNotNull(args, "args");

      if (IsDatasourceValid(args.Datasource))
      {
        Item contextItem = args.GetContextItem();
        if (contextItem != null)
        {
          List<SearchStringModel> model = new List<SearchStringModel>();
          model.AddRange(SearchStringModel.ParseDatasourceString(args.Datasource));
          model.AddRange(GetPageScope(contextItem));

          using (IProviderSearchContext searchContext = GetSearchContext(contextItem))
          {
            Item startLocationItem = contextItem.Database.GetItem(ItemIDs.RootID);

            IQueryable<ExtendedSearchResultItem> query = LinqHelper.CreateQuery<ExtendedSearchResultItem>(searchContext,
              model.RemoveWhere(m => m.Type == "sort"), startLocationItem);

            Item siteItem = MultisiteContext.GetSiteItem(contextItem);
            if (siteItem == null)
            {
              args.Datasource = string.Empty;
              return;
            }

            Item settingsItem = MultisiteContext.GetSettingsItem(siteItem);
            var datasourceSearchScopeIds = GetDatasourceSearchScopesIds(settingsItem);
            if (datasourceSearchScopeIds.Count == 0)
            {
              datasourceSearchScopeIds.Add(siteItem.ID.ToSearchID());
            }

            query = query.Where(BuildPathPredicate(datasourceSearchScopeIds));
            query = query.Where(i => i.Language == Context.Language.Name);
            query = AddSorting(model, query);
            query = query.Take(QueryMaxItems);

            ListString itemIds = new ListString(query.Select(r => r.ItemId.ToString()).ToList());
            args.Datasource = itemIds.ToString();
            return;
          }
        }

        args.Datasource = string.Empty;
      }
    }

    #region Fix290802
    protected virtual bool IsDatasourceValid(string datasource)
    {
      return datasource.Length > 1 &&
             datasource.Contains(":") &&
             !datasource.StartsWith(LocalDatasources.Constants.LocalPrefix) &&
             !datasource.StartsWith(LocalDatasources.Constants.PageRelativePrefix) &&
             !datasource.StartsWith(LocalDatasources.Constants.FieldPrefix) &&
             !datasource.StartsWith(LocalDatasources.Constants.CodePrefix);
    }
    #endregion

    protected virtual IQueryable<ExtendedSearchResultItem> AddSorting(List<SearchStringModel> model, IQueryable<ExtendedSearchResultItem> query)
    {
      foreach (SearchStringModel sort in model.Where(m => m.Type == "sort"))
      {
        string key = sort.Value.EndsWith("[desc]", StringComparison.OrdinalIgnoreCase) ? sort.Value.Substring(0, sort.Value.Length - "[desc]".Length).Trim() : sort.Value.Trim();
        Item facetItem = FacetService.GetFacetItems(new[] { key }, Context.Site.SiteInfo.Name).FirstOrDefault();
        bool floatFacet = false;
        bool integerFacet = false;
        bool dateFacet = false;

        if (facetItem != null)
        {
          floatFacet = facetItem.DoesItemInheritFrom(Templates.FloatFacet.ID);
          integerFacet = facetItem.DoesItemInheritFrom(Templates.IntegerFacet.ID);
          dateFacet = facetItem.DoesItemInheritFrom(Templates.DateFacet.ID);
        }

        if (sort.Value.EndsWith("[desc]", StringComparison.OrdinalIgnoreCase))
        {
          if (floatFacet)
          {
            query = query.OrderByDescending(i => i.get_Item<double>(key));
          }
          else if (integerFacet)
          {
            query = query.OrderByDescending(i => i.get_Item<long>(key));
          }
          else if (dateFacet)
          {
            query = query.OrderByDescending(i => i.get_Item<DateTime>(key));
          }
          else
          {
            query = query.OrderByDescending(i => i[key]);
          }
        }
        else
        {
          if (floatFacet)
          {
            query = query.OrderBy(i => i.get_Item<double>(key));
          }
          else if (integerFacet)
          {
            query = query.OrderBy(i => i.get_Item<long>(key));
          }
          else if (dateFacet)
          {
            query = query.OrderByDescending(i => i.get_Item<DateTime>(key));
          }
          else
          {
            query = query.OrderBy(i => i[key]);
          }
        }
      }

      return query;
    }
  }
  }
