import * as React from 'react';

import Modal from './modal';

interface IProps {
    loading: boolean;
}

export default class LoadingModal extends React.Component<IProps, {}> {
  
    public render() {
        if (!this.props.loading) {
            return null;
        }

        return (
            <Modal isOpen={true}>
                <div className="modal-body">
                    Loading...
                </div>
            </Modal>
        );
    }
}
