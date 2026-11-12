using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace HSMS.MVC.Common;

/// <summary>
/// Bind số thập phân từ form theo InvariantCulture (ô nhập type="number" luôn gửi dấu chấm), nên "8.5" luôn là 8,5
/// kể cả khi máy chủ chạy culture vi-VN (dấu thập phân là dấu phẩy). Chấp nhận thêm "8,5" gõ tay.
/// Dùng cho decimal và decimal?: gắn [ModelBinder(typeof(InvariantDecimalModelBinder))] lên thuộc tính.
/// </summary>
public class InvariantDecimalModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);

        var isNullable = Nullable.GetUnderlyingType(bindingContext.ModelType) != null;
        var valueResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

        if (valueResult == ValueProviderResult.None)
        {
            if (isNullable)
                return Task.CompletedTask;

            bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, "Vui lòng nhập điểm.");
            return Task.CompletedTask;
        }

        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueResult);

        var raw = valueResult.FirstValue?.Trim();
        if (string.IsNullOrEmpty(raw))
        {
            if (isNullable)
                bindingContext.Result = ModelBindingResult.Success(null);
            else
                bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, "Vui lòng nhập điểm.");

            return Task.CompletedTask;
        }

        if (decimal.TryParse(raw.Replace(',', '.'), NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture, out var value))
        {
            bindingContext.Result = ModelBindingResult.Success(value);
        }
        else
        {
            bindingContext.ModelState.TryAddModelError(bindingContext.ModelName, "Điểm số không hợp lệ.");
        }

        return Task.CompletedTask;
    }
}
