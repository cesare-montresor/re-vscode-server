
export namespace RE{
    
    export type ScriptStatus = "init" | "stop" | "run" | "pause" 

    export class Script{
        static DEBUG = true

        public fullpath = ""
        public content = ""
        public status:ScriptStatus = "init"
        public loop = false
        public autostart = false
        public hotkey = ''
    }
}


// Instantiate the class to start the WebSocket connection.