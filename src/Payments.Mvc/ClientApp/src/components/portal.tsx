import * as React from 'react';
import * as ReactDOM from 'react-dom';

const canUseDOM = !!(
    typeof window !== 'undefined' &&
    window.document &&
    window.document.createElement
  );

interface IProps {
    node: Element;
    children?: React.ReactNode;
}

export default class Portal extends React.Component<IProps, {}> {
    private defaultNode: Element;

    public componentWillUnmount() {
        if (this.defaultNode) {
            document.body.removeChild(this.defaultNode);
        }
        this.defaultNode = null;
    }

    public render() {
        if (!canUseDOM) {
        return null;
        }

        if (!this.props.node && !this.defaultNode) {
            this.defaultNode = document.createElement('div');
            document.body.appendChild(this.defaultNode);
        }

        return ReactDOM.createPortal(
            this.props.children,
            this.props.node || this.defaultNode
        );
    }
}
