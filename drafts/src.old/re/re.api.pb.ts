import * as vscode from 'vscode';
import * as protobuf from 'protobufjs';
import * as net from 'net';

import { EventEmitter } from 'stream';


export class API_PB{
    static DEBUG = true

    public static readonly PROTO_DEF = protobuf.loadSync(__dirname + '/../proto/open_file.proto');
    
    readonly defaultUri = API_PB.DEBUG ? "http://localhost:8080/" : "https://localhost:8080/"
    
    private pbUri: string = this.defaultUri;
    //private output!: HTMLElement;
    //this.output = document.getElementById("output") as HTMLElement;
    //window.addEventListener("load", () => this.init(), false);
    
    
    
    readonly onOpen = new EventEmitter()
    readonly onClose = new EventEmitter()
    readonly onMessage = new EventEmitter()
    readonly onError = new EventEmitter()
    



    private openFileProto?: protobuf.Type;
    private server: net.Server | undefined; // Server declared at module level
    private sockets: net.Socket[] = []; // To track connected sockets


    static getAPI(url?:string){
        return new API_PB(url);
    }

    

    constructor(url?:string) {
        if (url) this.pbUri = url

        this.server = net.createServer((socket) => {
            socket.on('connect', this.didOpen )
            socket.on('end', this.didClose )
            socket.on('data', this.didReceiveMessage )            
            socket.on('error', this.didError )
        })
        
        this.server.listen(8765, () => {
            console.log('RazorEnhanced Protobuf receiver listening on port 8888');
            vscode.window.showInformationMessage('RazorEnhanced Protobuf receiver started on port 8888');
        });
    }



    async startProtobufReceiver() {
        // Load the proto file
        
        

        // Start a TCP server
        const server = net.createServer((socket) => {
            socket.on('data', (data) => {
                
            });
        });

        server.listen(8888, () => {
            console.log('RazorEnhanced Protobuf receiver listening on port 8888');
            vscode.window.showInformationMessage('RazorEnhanced Protobuf receiver started on port 8888');
        });
    }

    public Close(): void {
        this.output("CLOSING ...");
        this.server?.close()
        // trigger the emit ? 
    }

    async stopProtobufReceiver() {
        if (this.server) {
            // Close all open sockets
            this.sockets.forEach(socket => socket.destroy());
            this.sockets = []; // Clear the socket list

            // Close the server
            this.server.close(() => {
                console.log('RazorEnhanced Protobuf receiver stopped');
                vscode.window.showInformationMessage('RazorEnhanced Protobuf receiver stopped');
            });

            this.server = undefined;
        } else {
            vscode.window.showInformationMessage('No receiver is running');
        }
    }




    public Send(message: string): void {
        this.output("SENT: " + message);
        this.server?.send(message);
    }

    



    private didOpen(): void {
        this.onOpen.emit("CONNECTED")
        this.output("CONNECTED");
        this.Send("WebSocket rocks");
    }

    private didClose(): void {
        this.onClose.emit("DISCONNECTED")
        this.output("DISCONNECTED");
    }

    private didError(error: Error): void {
        this.onError.emit("ERROR:"+error.message)
        this.output('<span style="color: red;">ERROR:'+error.message+'</span> ');
    }

    private didReceiveMessage(data:Buffer): void {
        var openFileProto = API_PB.PROTO_DEF.lookupType('OpenFileCommand');
        API_PB.PROTO_DEF.
        
        try {
            const message = openFileProto.decode(data);
        } catch (error) {
            console.error('Error decoding protobuf message:', error);
        }
        this.output('<span style="color: blue;">RESPONSE: ' + evt.data + '</span>');
        this.onMessage.emit(String(evt.data))
    }

    
    private output(message: string){
        if (API_WS.DEBUG){
            console.log(message)
        }

    }
}


// Instantiate the class to start the WebSocket connection.