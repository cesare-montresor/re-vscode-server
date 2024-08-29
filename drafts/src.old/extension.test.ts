import * as vscode from 'vscode';
import * as protobuf from 'protobufjs';
import * as net from 'net';
import { API_WS } from './re/re.api.ws'
import * as RE from './re/re.types'
import {Core} from './re/re.core'

let openFileProto: protobuf.Type;
let server: net.Server | undefined; // Server declared at module level
let sockets: net.Socket[] = []; // To track connected sockets

export function activate(context: vscode.ExtensionContext) {
    Core.runtime
    Core.activate(context: vscode.ExtensionContext)
}

export function deactivate() {
    RE.activate(context: vscode.ExtensionContext)
}

