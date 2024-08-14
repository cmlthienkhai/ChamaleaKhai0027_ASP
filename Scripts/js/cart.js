$(document).ready(function () {
    $('.add-to-cart').click(function () {
        var productId = $(this).data('product-id');
        var productName = $(this).data('product-name');
        var productImage = $(this).data('product-image');
        var productPrice = $(this).data('product-price');
        var quantity = 1; // Giá trị số lượng mặc định, có thể thay đổi tùy theo yêu cầu

        var model = {
            ProductId: productId,
            ProductName: productName,
            ProductImage: productImage,
            Quantity: quantity,
            Price: productPrice
        };

        $.ajax({
            type: "POST",
            url: '/Cart/AddToCart',
            data: JSON.stringify(model),
            dataType: "json",
            contentType: "application/json; charset=utf-8",
            success: function (response) {
                alert(response.Message);
            },
            error: function (xhr, status, error) {
                alert("Lỗi khi thêm vào giỏ hàng: " + xhr.responseText);
            }
        });
    });
});
