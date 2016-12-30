var bodyParser = require('body-parser');

var express = require('express');
  //config = require('./config/config');

var app = express();

//module.exports = require('./config/express')(app, config);
app.use(bodyParser.json());
app.use(bodyParser.urlencoded({extended:true}));
  console.log(req.body);

app.listen(3000, function () {
  console.log('Express server listening on port 3000');
});

