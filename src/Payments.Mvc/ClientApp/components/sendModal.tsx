import * as React from 'react';

import { PreviewInvoice } from '../models/PreviewInvoice';
import { Team } from '../models/Team';

import Modal from './modal';
import PreviewFrame from './previewFrame';

interface IProps {
    invoice: PreviewInvoice;
    team: Team;
    isModalOpen: boolean;

    onCancel: () => void;
    onSend: (ccEmails: string) => void;
}

interface IState {
    ccEmails: string;
}

export default class SendModal extends React.Component<IProps, IState> {

    constructor(props) {
        super(props);

        this.state = {
            ccEmails: '',
        };
    }

    public render() {
        const { isModalOpen, onCancel } = this.props;

        return (
            <Modal dialogClassName="send-invoice-modal modal-lg" isOpen={isModalOpen} onBackdropClick={onCancel} onEscape={onCancel}>
                <div className="modal-header">
                    <div className="row flex-grow-1">
                        <div className="col-md-3" />
                        <div className="col-md-6 d-flex justify-content-center align-items-center">
                            <h3 className="modal-title">Send Invoice</h3>
                        </div>
                        <div className="col-md-3 d-flex justify-content-end align-items-center">
                            <button type="button" className="close m-1" onClick={onCancel}>
                                <span aria-hidden="true"><i className="fas fa-times" /></span>
                            </button>
                        </div>
                    </div>
                </div>
                { this.renderSendBody() }
                { this.renderPreviewFrame() }
                <div className="modal-footer">
                    <div className="flex-grow-1 d-flex justify-content-between align-items-center">
                        <span><i className="fas fa-info mx-3" /> This invoice can't be edited after it's sent.</span>
                        <div className="d-flex align-items-baseline">
                            <button className="btn-plain mr-3" onClick={onCancel}>Cancel</button>
                            <button className="btn" onClick={this.onSend}>Send Invoice</button>
                        </div>
                    </div>
                </div>
            </Modal>
        );
    }

    private renderPreviewFrame() {
        const { invoice, team } = this.props;

        return (
            <div className="modal-body p-0">
                <PreviewFrame
                    invoice={invoice}
                    team={team}
                />
            </div>
        );
    }

    private renderSendBody() {
        const email = this.props.invoice.customerEmail

        return (
            <div className="modal-body send-invoice-modal-body">
                <div className="form-group row">
                    <label htmlFor="sendEmail" className="col-3 col-form-label text-right">Send to</label>
                    <div className="col-8">
                        <input type="text" className="form-control" name="sendEmail" readOnly={true} value={email} />
                    </div>
                </div>
                <div className="form-group row">
                    <label htmlFor="cc" className="col-3 col-form-label text-right">Cc</label>
                    <div className="col-8">
                        <input
                            type="text"
                            className="form-control"
                            name="cc"
                            value={this.state.ccEmails} onChange={(e) => this.onCcEmailChange(e.target.value)} />
                    </div>
                </div>
            </div>
        );
    }

    private onCcEmailChange = (value) => {
        this.setState({ ccEmails: value });
    }

    private onSend = () => {
        // TODO: Validate emails
        const ccEmails = this.state.ccEmails;

        this.props.onSend(ccEmails);
    }
}