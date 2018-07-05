import * as React from 'react';

import Portal from './Portal';

interface IProps {
    dialogClassName?: string;
    isOpen: boolean;

    onOpened?: () => void;
    onClosed?: () => void;

    onBackdropClick?: () => void;
    onEscape?: () => void;
}

interface IState {
    isOpen: boolean;
}

export default class LoadingModal extends React.Component<IProps, IState> {
  
    private _portal: HTMLDivElement;
    private _dialog: HTMLDivElement;
    private _mouseDownElement: HTMLElement;
    private _isMounted: boolean = false;

    constructor(props) {
        super(props);

        this.state = {
            isOpen: props.isOpen,
        };

        if (props.isOpen) {
            this.init();
        }
    }

    public componentDidMount() {
        this._isMounted = true;
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

        this._isMounted = false;
    }

    public render() {
        if (!this.state.isOpen) {
            return null;
        }

        return (
            <Portal node={this._portal}>
                <div
                    className="modal"
                    style={{ display: "block" }}
                    onKeyUp={this.handleEscape}
                    onMouseDown={this.handleBackdropMouseDown}
                    onMouseUp={this.handleBackdropMouseUp}
                >
                    {this.renderModalDialog()}
                </div>
                <div className="modal-backdrop fade show" />
            </Portal>
        );
    }
    
    private renderModalDialog() {
        const { dialogClassName } = this.props;


        return (
            <div className={`modal-dialog modal-dialog-centered ${dialogClassName}`} role="document" ref={r => this._dialog = r}>
                <div className="modal-content">
                    { this.props.children }
                </div>
            </div>
        );
    }

    private handleEscape = (e) => {
        if (this.props.isOpen && e.keyCode === 27 && this.props.onEscape) {
          this.props.onEscape();
        }
    }

    private handleBackdropMouseDown = (e) => {
        this._mouseDownElement = e.target;
    }

    private handleBackdropMouseUp = (e) => {
        if (e.target === this._mouseDownElement) {
          e.stopPropagation();
          if (!this.props.isOpen) {
              return;
          }
    
          const container = this._dialog;
    
          if (e.target && !container.contains(e.target) && this.props.onBackdropClick) {
            this.props.onBackdropClick();
          }
        }
    }

    private init() {
        // create portal
        this._portal = document.createElement('div');
        this._portal.setAttribute('tabindex', '-1');
        this._portal.style.position = "relative";
        this._portal.style.zIndex = "100";
        
        // add to end of document
        document.body.appendChild(this._portal);

        // add body class
        document.body.className = document.body.className + " modal-open";
    }

    private destroy() {
        // remove portal
        if (this._portal) {
            document.body.removeChild(this._portal);
            this._portal = null;
        }

        // remove modal open class, match exact
        const modalOpenClassNameRegex = new RegExp(`(^| )modal-open( |$)`);
        document.body.className = document.body.className.replace(modalOpenClassNameRegex, ' ').trim();
    }

    private onOpen() {
        if (this.props.onOpened) {
            this.props.onOpened();
        }
    }

    private onClose() {
        if (this.props.onClosed) {
            this.props.onClosed();
        }

        this.destroy();

        if (this._isMounted) {
            this.setState({ isOpen: false });
        }
    }
}
