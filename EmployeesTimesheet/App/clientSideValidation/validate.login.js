$(document).ready(function () {
    $("#loginForm").validate({
        rules: {
           
            Username: {
                required: true,
                minlength: 4
            },
            Password: {
                required: true,
                minlength: 4
            }
            
        },
        messages: {
            Username: {
                required: "Please enter a username",
                minlength: "Your username must consist of at least 4 characters"
            },
            Password: {
                required: "Please provide a password",
                minlength: "Your password must be at least 4 characters long"
            }
         
        }
       
    });

});