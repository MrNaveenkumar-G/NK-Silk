// NK Silk — front-end cart interactions (jQuery + AJAX)
window.NKSilk = (function ($) {
    "use strict";

    function token() {
        return $('input[name="__RequestVerificationToken"]').first().val();
    }

    function fmt(n) {
        return '₹' + Number(n).toLocaleString('en-IN', { maximumFractionDigits: 0 });
    }

    function setBadge(count) {
        $('#cart-count').text(count);
    }

    function refreshBadge() {
        $.get('/Cart/Count', function (res) { setBadge(res.itemCount); });
    }

    function addToCart(variantId, qty, msgEl) {
        $.ajax({
            url: '/Cart/Add',
            method: 'POST',
            data: { productVariantId: variantId, quantity: qty, __RequestVerificationToken: token() }
        }).done(function (res) {
            if (res.success) {
                setBadge(res.itemCount);
                if (msgEl) { msgEl.className = 'small mt-2 text-success'; msgEl.textContent = 'Added to cart ✓'; }
            } else if (msgEl) {
                msgEl.className = 'small mt-2 text-danger'; msgEl.textContent = res.message || 'Could not add item.';
            }
        }).fail(function () {
            if (msgEl) { msgEl.className = 'small mt-2 text-danger'; msgEl.textContent = 'Something went wrong.'; }
        });
    }

    function initCartPage() {
        // Quantity change
        $('.js-qty').on('change', function () {
            var row = $(this).closest('tr');
            var id = row.data('item-id');
            var qty = parseInt($(this).val()) || 1;
            $.ajax({
                url: '/Cart/Update', method: 'POST',
                data: { cartItemId: id, quantity: qty, __RequestVerificationToken: token() }
            }).done(function (res) {
                setBadge(res.itemCount);
                $('#cartSubtotal').text(fmt(res.subTotal));
                location.reload(); // simplest way to refresh line totals
            });
        });

        // Remove line
        $('.js-remove').on('click', function () {
            var row = $(this).closest('tr');
            var id = row.data('item-id');
            $.ajax({
                url: '/Cart/Remove', method: 'POST',
                data: { cartItemId: id, __RequestVerificationToken: token() }
            }).done(function (res) {
                setBadge(res.itemCount);
                row.remove();
                $('#cartSubtotal').text(fmt(res.subTotal));
                if (res.itemCount === 0) location.reload();
            });
        });
    }

    function setWishlistBadge(count) {
        var el = $('#wishlist-count');
        if (!el.length) return;
        el.text(count);
        el.toggle(count > 0);
    }

    function refreshWishlistBadge() {
        if (!$('#wishlist-count').length) return;
        $.get('/Wishlist/Count', function (res) { setWishlistBadge(res.count); });
    }

    function toggleWishlist(productId, done) {
        $.ajax({
            url: '/Wishlist/Toggle',
            method: 'POST',
            data: { productId: productId, __RequestVerificationToken: token() }
        }).done(function (res) { if (res.success && done) done(res); });
    }

    function refreshNotificationBadge() {
        var el = $('#notification-count');
        if (!el.length) return;
        $.get('/Notifications/Count', function (res) {
            el.text(res.count);
            el.toggle(res.count > 0);
        });
    }

    $(refreshBadge);
    $(refreshWishlistBadge);
    $(refreshNotificationBadge);

    return {
        addToCart: addToCart,
        initCartPage: initCartPage,
        refreshBadge: refreshBadge,
        toggleWishlist: toggleWishlist,
        setWishlistBadge: setWishlistBadge,
        refreshNotificationBadge: refreshNotificationBadge
    };
})(jQuery);
