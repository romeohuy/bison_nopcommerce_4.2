using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Integration.KiotViet.Integration.KiotViet
{
   public class KiotVietConstant
   {
       public const string UrlApiRootToken = "https://id.kiotviet.vn/";
       public const string UrlApiRootPublic = "https://public.kiotapi.com/";
       public const string UrlApiGetToken = "connect/token";
       public const string UrlApiGetCategory = "categories?currentItem=&pageSize=100&hierachicalData=true&orderby=categoryName";
       public const string UrlApiGetProduct = "Products?lastModifiedFrom=&orderBy=modifiedDate&orderDirection=desc&includeRemoveIds=true&includeInventory=true&includePricebook=false&currentItem={0}&pageSize={1}";
   }
}
