using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.Primitives;
using MongoDB.Bson;

namespace DanceFloor.Api.Converters
{
    public class ObjectIdModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var result = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

            object objectId = null;
            
            if (string.IsNullOrWhiteSpace(result.FirstValue) || !ObjectId.TryParse(result.FirstValue, out var parsed))
            {
                objectId = Nullable.GetUnderlyingType(bindingContext.ModelType) != null ?
                    (ObjectId?)null : ObjectId.Empty;
            }
            else
            {
                objectId = parsed;
            }

            bindingContext.Result = ModelBindingResult.Success(objectId);
            
            return Task.CompletedTask;
        }
    }
    
    public class ObjectIdModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return context.Metadata.ModelType == typeof(ObjectId) || context.Metadata.ModelType == typeof(ObjectId?) ?
                new BinderTypeModelBinder(typeof(ObjectIdModelBinder)) :
                null;
        }
    }
}