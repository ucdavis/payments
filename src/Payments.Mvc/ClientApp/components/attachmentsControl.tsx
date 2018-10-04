import * as React from 'react';
import { compose, fromRenderProps } from 'recompose';

import FileUpload from '../components/fileUpload';

import { InvoiceAttachment } from '../models/InvoiceAttachment';
import { Team } from '../models/Team';

import TeamContext from '../contexts/TeamContext';


interface IProps {
    attachments: InvoiceAttachment[];
    team: Team;
    onChange: (value: InvoiceAttachment[]) => void;
}

interface IState {
}

class AttachmentsControl extends React.Component<IProps, IState> {

    public render() {
        const { attachments } = this.props;

        return (
            <div className='invoice-attachments-control'>
                <FileUpload className='file-upload' onFileUpload={this.onFileUpload} />
                { attachments.map(this.renderAttachment) }
            </div>
        )
    }

    private renderAttachment = (attachment: InvoiceAttachment) => {

        const { team } = this.props;

        const fileTypeIcon = this.getFileTypeIcon(attachment.contentType)

        const sizeText = this.getSizeText(attachment.size);

        const href = `${team.slug}/files/getfile/${attachment.identifier}?filename=${attachment.fileName}`;

        // break out extension
        const parts = attachment.fileName.split('.');
        const extension = parts.pop();
        const fileName = parts.join('.');

        return (
            <div key={attachment.identifier} className='invoice-attachment row justify-content-between align-items-center mb-3'>

                <div className='col-9 d-flex align-items-center'>
                    <i className={`fa-2x mr-4 ${fileTypeIcon}`} />
                    <div className='input-group'>
                        <input className='form-control' value={fileName} onChange={(e) => this.onFileNameChange(attachment.identifier, e)}/>
                        <div className='input-group-append'>
                            <span className='input-group-text'>.{ extension }</span>
                        </div>
                    </div>
                </div>
                
                <span className='col-2 d-flex justify-content-center'>{ sizeText }</span>

                <div className='col-1 d-flex justify-content-end align-items-center'>
                    <a href={href} className='btn-link btn-invoice-delete mx-2' target='_blank'>
                        <i className='fas fa-download' />
                    </a>

                    <button className='btn-link btn-invoice-delete ml-2' onClick={() => this.removeAttachment(attachment.identifier)}>
                        <i className='fas fa-trash-alt' />
                    </button>
                </div>

            </div>
        );
    }
    
    private getFileTypeIcon = (contentType: string) => {
        if (contentType === 'application/pdf') {
            return 'far fa-file-pdf';
        }

        if (contentType.startsWith('image')) {
            return 'far fa-file-image';
        }

        return 'far fa-file';
    }

    private getSizeText = (size: number) => {
        if (size <= 0) {
            return null;
        }

        if (size <= 1024) {
            return `${size.toFixed(0)} B`;
        }

        if (size <= 1024 * 1024) {
            return `${ (size / 1024).toFixed(0) } KB`;
        }

        return `${ (size / 1024 / 1024).toFixed(1) } MB`;
    }

    private onFileNameChange = (identifier: string, event: React.ChangeEvent<HTMLInputElement>) => {
        const { attachments, onChange } = this.props;

        // find and break out extension
        const index = attachments.findIndex(a => a.identifier === identifier);
        const parts = attachments[index].fileName.split('.');
        const extension = parts.pop();

        // reconstruct total filename
        attachments[index].fileName = `${event.target.value}.${extension}`;

        const newAttachments = [
            ...attachments,
        ];

        // push completed update
        onChange(newAttachments);
    }

    private removeAttachment = (identifier: string) => {
        const { attachments, onChange } = this.props;

        // find and remove attachment
        const newAttachments = [
            ...attachments.filter(a => a.identifier !== identifier),
        ];

        // push completed attachment up
        onChange(newAttachments);
    }
    
    private onFileUpload = (attachment: InvoiceAttachment) => {
        const { attachments, onChange } = this.props;

        const newAttachments = [
            ...attachments,
            attachment
        ];

        onChange(newAttachments);
    }
}

export default compose(
    fromRenderProps(TeamContext.Consumer, (team) => ({ team })),
)(AttachmentsControl);
