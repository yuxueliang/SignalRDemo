"use strict";


var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub",
    {
        accessTokenFactory: () => token // Return access token
        })
    //.withAutomaticReconnect()//自动重新连接别等待0、2、10和30秒，然后再尝试重新连接尝试。不知道为什么不能使用
    .withHubProtocol(new signalR.protocols.msgpack.MessagePackHubProtocol())
    .configureLogging(signalR.LogLevel.Information)//日志记录
    .build();//创建连接。

//Disable send button until connection is established
document.getElementById("sendButton").disabled = true;

connection.on("ReceiveMessage", function (user, message) {
    var msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    var encodedMsg = user + " says " + msg;
    var li = document.createElement("li");
    li.textContent = encodedMsg;
    document.getElementById("messagesList").appendChild(li);
});

connection.start().then(function () {//启动连接
    document.getElementById("sendButton").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("sendButton").addEventListener("click", function (event) {
    var user = document.getElementById("userInput").value;
    var message = document.getElementById("messageInput").value;
    connection.invoke("SendMessageToUser", user, message).catch(function (err) {
        return console.error(err.toString());//如果调用集线器中的方法出现异常，这里能够抓取到
    });
    event.preventDefault();
});

document.getElementById("sendButton1").addEventListener("click", function (event) {
    var user = document.getElementById("userInput").value;
    var message = document.getElementById("messageInput").value;
    connection.invoke("SendMessageToUser1", user, message).catch(function (err) {
        return console.error(err.toString());//如果调用集线器中的方法出现异常，这里能够抓取到
    })
    .then(result => {
        console.log(result);//得到服务器端返回的结果
    });
    event.preventDefault();
});

document.getElementById("addGroup").addEventListener("click", function (event) {
    var user = document.getElementById("userInput").value;
    var message = document.getElementById("messageInput").value;
    connection.invoke("AddToGroup", user).catch(function (err) {
            return console.error(err.toString());//如果调用集线器中的方法出现异常，这里能够抓取到
        })
        .then(result => {
            console.log(result);//得到服务器端返回的结果
        });
    event.preventDefault();
});

connection.on("Send", function (message) {
    var msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    var encodedMsg =  msg;
    var li = document.createElement("li");
    li.textContent = encodedMsg;
    document.getElementById("messagesList").appendChild(li);
});
document.getElementById("groupSendMessage").addEventListener("click", function (event) {
    var user = document.getElementById("userInput").value;
    var message = document.getElementById("messageInput").value;
    connection.invoke("SendToUserMessage", user, message).catch(function (err) {
            return console.error(err.toString());//如果调用集线器中的方法出现异常，这里能够抓取到
        })
        .then(result => {
            console.log(result);//得到服务器端返回的结果
        });
    event.preventDefault();
});



//connection.onreconnecting((error) => {
//    console.assert(connection.state === signalR.HubConnectionState.Reconnecting);

//    document.getElementById("messageInput").disabled = true;

//    const li = document.createElement("li");
//    li.textContent = `Connection lost due to error "${error}". Reconnecting.`;
//    document.getElementById("messagesList").appendChild(li);
//});