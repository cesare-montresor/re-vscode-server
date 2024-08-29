import * as vscode from 'vscode';
import * as protobuf from 'protobufjs';
import * as net from 'net';
import { WebSocket, Event, CloseEvent, MessageEvent } from 'ws';
import { EventEmitter } from 'stream';
import { Script } from 'vm';


namespace RE{
    

    export class Message{
        static DEBUG = true

        public msgid = -1
        public messageType = this.constructor.name
        public payload:any
    }

    export class MessageScripts extends Message{
        public payload = new Array<Script>()
    }

}

// Instantiate the class to start the WebSocket connection.