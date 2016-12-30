var bodyParser = require('body-parser');

var express = require('express');
  //config = require('./config/config');

var app = express();

//module.exports = require('./config/express')(app, config);
app.use(bodyParser.json());
app.use(bodyParser.urlencoded({extended:true}));
app.post('/rideshook', function(req,res,next){
  console.log(req.body);
  console.log(req.headers['content-type']);
  res.send('ping');
});


app.listen(3000, function () {
  console.log('Express server listening on port 3000');
});

