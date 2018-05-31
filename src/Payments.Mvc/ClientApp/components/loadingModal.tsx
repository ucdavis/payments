import * as React from 'react';

import Portal from './Portal';

interface IProps {
    isOpen: boolean;
}

interface IState {
    isOpen: boolean;
}

export default class LoadingModal extends React.Component<IProps, IState> {
  
    private _portal: HTMLDivElement;

    constructor(props) {
        super(props);

        this.state = {
            isOpen: props.isOpen,
        };

        if (props.isOpen) {
            this.init();
        }
    }

    public componentWillReceiveProps(nextProps) {
        if (nextProps.isOpen !== this.props.isOpen) {
            this.setState({ isOpen: nextProps.isOpen });
        }

        if (nextProps.isOpen) {
            this.onOpen();
        }
        else {
            this.onClose();
        }
    }

    public componentWillUpdate(nextProps, nextState) {
        // close -> open
        if (nextState.isOpen && !this.state.isOpen) {
            this.init();
        }
    }

    public componentWillUnmount() {
        if (this.state.isOpen) {
            this.destroy();
        }
    }

    public render() {
        if (!this.state.isOpen) {
            return null;
        }

        return (
            <Portal node={this._portal}>
                <div className="modal" style={{ display: "block" }}>
                    {this.renderModalDialog()}
                </div>
                <div className="modal-backdrop fade show" />
            </Portal>
        );
    }
    
    private renderModalDialog() {
        return (
            <div className= "modal-dialog modal-dialog-centered" role="document">
                <div className="modal-content">
                    <div className="modal-body">
                        Loading ...
                    </div>
                </div>
            </div>
        );
    }

    private init() {
        // create portal
        this._portal = document.createElement('div');
        this._portal.setAttribute('tabindex', '-1');
        this._portal.style.position = "relative";
        this._portal.style.zIndex = "100";
        
        // add to end of document
        document.body.appendChild(this._portal);
    }

    private destroy() {
        // remove portal
        if (this._portal) {
            document.body.removeChild(this._portal);
            this._portal = null;
        }
    }

    private onOpen() {
        // add body class
        document.body.className = document.body.className + " modal-open";
    }

    private onClose() {
        // remove modal open class, match exact
        const modalOpenClassNameRegex = new RegExp(`(^| )modal-open( |$)`);
        document.body.className = document.body.className.replace(modalOpenClassNameRegex, ' ').trim();
    }
}
