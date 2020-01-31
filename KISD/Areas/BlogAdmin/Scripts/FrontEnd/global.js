
// Smooth Scroll to Top
$(document).ready(function() {
  var offset = 220;
  var duration = 500;
  $(window).scroll(function() {
    if ($(this).scrollTop() > offset) {
      $('.scroll-top').fadeIn(duration);
    } else {
      $('.scroll-top').fadeOut(duration);
    }
  });
$('.scroll-top').click(function(event) {
    event.preventDefault();
    $('html, body').animate({scrollTop: 0}, duration);
    return false;
  })
});