const WebSocket = require('ws');

const ws = new WebSocket.Server({port : 5000});

let players = {};
let idents = {};
let playersOnServer = 0;

ws.on('connection', connected => {

  playersOnServer++;
  let uniqueId = Date.now();
  idents[connected] = uniqueId;

  connected.on('message', data => {
    players[uniqueId] = data;
  });

  connected.on('close', (code, reason) => {
    delete players[uniqueId];
    playersOnServer--;
  });

});

//broadcaster

setInterval( () => {
  ws.clients.forEach(client => {

    let package = JSON.stringify({
      id: idents[client].toString(),
      count: playersOnServer,
      info: players
    });
    client.send(package);
  });
}, 40);

//log current server state

setInterval( () => {
  console.log({
    online: playersOnServer,
    playersInfo: players
  });
}, 2500 );