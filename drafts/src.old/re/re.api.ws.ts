import { WebSocket, Event, CloseEvent, MessageEvent, ErrorEvent } from 'ws';
import { EventEmitter } from 'stream';


export class API_WS{
    static DEBUG = true
    
    readonly defaultUri = API_WS.DEBUG ? "ws://localhost:8080/" : "wss://localhost:8080/"
    
    private wsUri: string = this.defaultUri;
    //private output!: HTMLElement;
    //this.output = document.getElementById("output") as HTMLElement;
    //window.addEventListener("load", () => this.init(), false);
    private websocket!: WebSocket

    
    readonly onOpen = new EventEmitter()
    readonly onClose = new EventEmitter()
    readonly onMessage = new EventEmitter()
    readonly onError = new EventEmitter()

    

    static getAPI(url?:string){
        return new API_WS(url);
    }

    constructor(url?:string) {
        if (url) this.wsUri = url

        this.websocket = new WebSocket(this.wsUri);
        this.websocket.onopen = (evt) => this.didOpen(evt);
        this.websocket.onclose = (evt) => this.didClose(evt);
        this.websocket.onmessage = (evt) => this.didReceiveMessage(evt);
        this.websocket.onerror = (evt) => this.didError(evt);
    }

    public Send(message: string): void {
        this.output("SENT: " + message);
        this.websocket.send(message);
    }

    public Close(): void {
        this.output("CLOSING ...");
        this.websocket.close()
    }



    private didOpen(evt: Event): void {
        this.onOpen.emit("CONNECTED")
        this.output("CONNECTED");
        this.Send("WebSocket rocks");
    }

    private didClose(evt: CloseEvent): void {
        this.onClose.emit("DISCONNECTED")
        this.output("DISCONNECTED");
    }

    private didError(evt: ErrorEvent): void {
        this.onError.emit("ERROR")
        this.output('<span style="color: red;">ERROR:</span> ');
    }

    private didReceiveMessage(evt: MessageEvent): void {
        this.onMessage.emit(String(evt.data))
        this.output('<span style="color: blue;">RESPONSE: ' + evt.data + '</span>');
    }

    
    private output(message: string){
        if (API_WS.DEBUG){
            console.log(message)
        }

    }
}


// Instantiate the class to start the WebSocket connection.