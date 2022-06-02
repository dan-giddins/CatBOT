const generalId = "673983768557649935";

var fs = require("fs");
var Discord = require("discord.io");
var logger = require("winston");
var auth = require("./auth.json");
const { join } = require("path");

// Configure logger settings
logger.remove(logger.transports.Console);
logger.add(new logger.transports.Console(), {
  colorize: true,
});
logger.level = "debug";

// Initialize Discord Bot
var bot = new Discord.Client({
  token: auth.token,
  autorun: true,
});

// main
bot.on("ready", function (evt) {
  logger.info("Connected as " + bot.username + " - (" + bot.id + ")");
  bot.sendMessage({
    to: generalId,
    message: "*meows happily*",
  });
  setInterval(function () {
    bot.sendMessage({
      to: generalId,
      message: "*meows randomly*",
    });
  }, 1000000 * Math.random());
});

// on message
bot.on("message", function (user, userID, channelID, message, evt) {
  // Our bot needs to know if it will execute a command
  // It will listen for messages that will start with `!`
  if (message.substring(0, 1) == "!") {
    var args = message.substring(1).split(" ");
    var cmd = args[0];
    args = args.splice(1);
    switch (cmd) {
      // !ping
      case "meow":
        console.log(channelID);
        bot.sendMessage({
          to: channelID,
          message: "*meows back at you*",
        });
        break;
      // Just add any case commands if you want to..
    }
  }
});

// on event
bot.on("any", function (event) {
  switch (event.t) {
    case "MESSAGE_DELETE":
      bot.sendMessage({
        to: event.d.channel_id,
        message: "*meows sadly*",
      });
      break;
    case "VOICE_STATE_UPDATE":
      bot.sendMessage({
        to: generalId,
        message: "*meows inquisitively*",
      });
      var voiceChannelID = event.d.channel_id;
      console.log("test1");
      //Let's join the voice channel, the ID is whatever your voice channel's ID is.
        bot.joinVoiceChannel(voiceChannelID, function (error, events) {
          console.log("test2");
          //Check to see if any errors happen while joining.
          if (error) return console.error(error);
          //Then get the audio context
          client.getAudioContext(voiceChannelID, function (error, stream) {
            console.log("test3");
            //Once again, check to see if any errors exist
            if (error) return console.error(error);
            //Create a stream to your file and pipe it to the stream
            //Without {end: false}, it would close up the stream, so make sure to include that.
            fs.createReadStream("meow.m4a").pipe(stream, { end: false });
            console.log("test4");
            //The stream fires `done` when it's got nothing else to send to Discord.
            stream.on("done", function () {
              //Handle
              console.log("test5");
            });
          });
        });
      }
      break;
  }
});
