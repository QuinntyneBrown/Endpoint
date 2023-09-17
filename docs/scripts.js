class App extends HTMLElement {

    connectedCallback() {
        console.log("Endpoint App");
    }
}

window.customElements.define("endpoint-app", App);