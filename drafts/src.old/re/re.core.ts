import * as vscode from 'vscode';
import * as net from 'net';
import {API_WS} from './re.api.ws'
import {API_PB} from './re.api.pb'
   

export type ProtocolType = "ws"|"pb"

export class Core{
    
    public static readonly Runtime = new Core()

    public protocol: ProtocolType = "ws"
    public api_ws?: API_WS
    public api_bp?: API_PB


    constructor(method?:ProtocolType){
        if (method)this.protocol = method
        
    }

    activate(context: vscode.ExtensionContext) {
        console.log('RazorEnhanced extension is now active!');
    
        
        let startReceiver = vscode.commands.registerCommand('razorenhanced.startReceiver', () => {
            this.startReceiverWS();
            this.startReceiverPB();
        });
    
        let stopReceiver = vscode.commands.registerCommand('razorenhanced.stopReceiver', () => {
            this.stopReceiverWS()
            this.stopReceiverPB();
        });
    
        context.subscriptions.push(startReceiver);
        context.subscriptions.push(stopReceiver);
    }

    deactivate() {
        this.stopReceiverWS()
        this.stopReceiverPB();
        console.log('RazorEnhanced extension is deactivated.');
    }


    async startReceiverWS() {
        this.api_ws = API_WS.getAPI()
    }

    async stopReceiverWS() {
        this.api_ws?.Close()
    }
    
    async startReceiverPB() {
        this.api_bp = API_PB.getAPI()
        // Load the proto file
    }


    async stopReceiverPB() {
        this.api_bp?.Close();
    }


    handleOpenFileCommand(command: any) {
        const filePath = command.filePath;
        vscode.workspace.openTextDocument(filePath).then((document) => {
            vscode.window.showTextDocument(document);
        }, (error) => {
            vscode.window.showErrorMessage(`Failed to open file: ${error}`);
        });
    }
}


// Instantiate the class to start the WebSocket connection.