import * as React from 'react';

import axios, { AxiosResponse, CancelTokenSource } from 'axios';
import Dropzone, { DropzoneRenderArgs } from 'react-dropzone';

import { InvoiceAttachment } from '../models/InvoiceAttachment';

import TeamContext from '../contexts/TeamContext';

import { uuidv4 } from '../utils/string';

declare var antiForgeryToken: string;

interface UploadingInvoiceAttachment extends InvoiceAttachment {
  cancelToken?: CancelTokenSource;
  progress: number;
}

interface IProps {
  className?: string;
  onFileUpload: (file: InvoiceAttachment) => void;
}

interface IState {
  attachmentsUploading: UploadingInvoiceAttachment[];
}

export default class FileUpload extends React.Component<IProps, IState> {
  static contextType = TeamContext;

  constructor(props) {
    super(props);

    this.state = {
      attachmentsUploading: []
    };
  }

  public render() {
    const { className } = this.props;
    const { attachmentsUploading } = this.state;

    return (
      <div className={className}>
        <Dropzone onDrop={this.startUpload}>{this.renderDropzone}</Dropzone>

        {attachmentsUploading.map(this.renderUploadingAttachment)}
      </div>
    );
  }

  private renderDropzone = (args: DropzoneRenderArgs) => {
    return (
      <div {...args.getRootProps()} className='dropzone'>
        <input {...args.getInputProps()} />
        <div className='d-flex justify-content-center align-items-center'>
          <i className='fas fa-upload fa-2x me-4' />
          <div className='d-flex flex-column align-items-center'>
            {args.isDragActive ? (
              <span>Drop files here...</span>
            ) : (
              <span>Drop files to attach, or click to Browse.</span>
            )}
            <span>(Individual file upload size limit 5 MB)</span>
          </div>
        </div>
      </div>
    );
  };

  private renderUploadingAttachment = (
    attachment: UploadingInvoiceAttachment
  ) => {
    const fileTypeIcon = this.getFileTypeIcon(attachment.contentType);

    const sizeText = this.getSizeText(attachment.size);

    const progress = attachment.progress;

    return (
      <div
        key={attachment.fileName}
        className='invoice-attachment d-flex justify-content-between align-items-center mb-3'
      >
        <i className={'fa-2x ' + fileTypeIcon} />

        <div className='d-flex flex-column flex-grow-1 mx-4'>
          <span>{attachment.fileName}</span>
          {this.renderProgressBar(progress)}
          <span>
            {sizeText} ({progress}% is done)
          </span>
        </div>

        <button
          className='btn btn-danger'
          onClick={() => this.cancelUpload(attachment)}
        >
          <i className='fas fa-times' />
        </button>
      </div>
    );
  };

  private getFileTypeIcon = (contentType: string) => {
    if (contentType === 'application/pdf') {
      return 'far fa-file-pdf';
    }

    if (contentType.startsWith('image')) {
      return 'far fa-file-image';
    }

    return 'far fa-file';
  };

  private renderProgressBar = (progress: number) => {
    const style: React.CSSProperties = {
      width: `${progress}%`
    };

    return (
      <div className='progress'>
        <div
          className='progress-bar progress-bar-striped progress-bar-animated'
          role='progressbar'
          style={style}
        />
      </div>
    );
  };

  private getSizeText = (size: number) => {
    if (size <= 0) {
      return null;
    }

    if (size <= 1024) {
      return `${size.toFixed(0)} B`;
    }

    if (size <= 1024 * 1024) {
      return `${(size / 1024).toFixed(0)} KB`;
    }

    return `${(size / 1024 / 1024).toFixed(1)} MB`;
  };

  private startUpload = (accepted: File[], rejected, event) => {
    const { slug } = this.context;

    // start uploads
    for (const file of accepted) {
      const newAttachment: UploadingInvoiceAttachment = {
        cancelToken: axios.CancelToken.source(),
        contentType: file.type,
        fileName: file.name,
        identifier: uuidv4(),
        progress: 0,
        size: file.size
      };

      const newAttachments = [
        ...this.state.attachmentsUploading,
        newAttachment
      ];

      this.setState({
        attachmentsUploading: newAttachments
      });

      const data = new FormData();
      data.append('file', file);

      axios(`${slug}/files/uploadfile`, {
        cancelToken: newAttachment.cancelToken.token,
        data,
        headers: {
          RequestVerificationToken: antiForgeryToken
        },
        method: 'post',
        onUploadProgress: progressEvent =>
          this.onUploadProgress(progressEvent, newAttachment.identifier)
      })
        .then(response => this.onUploadComplete(response, newAttachment))
        .catch(error => console.error(error));
    }
  };

  private onUploadProgress = (
    progressEvent: ProgressEvent,
    identifier: string
  ) => {
    const attachmentsUploading = [...this.state.attachmentsUploading];

    // find and update attachment
    const index = attachmentsUploading.findIndex(
      a => a.identifier === identifier
    );
    if (index > -1) {
      attachmentsUploading[index].progress =
        (progressEvent.loaded / progressEvent.total) * 100;

      this.setState({
        attachmentsUploading
      });
    }
  };

  private cancelUpload = (attachment: UploadingInvoiceAttachment) => {
    // stop upload
    if (attachment.cancelToken) {
      attachment.cancelToken.cancel();
    }

    const { attachmentsUploading } = this.state;

    // find and remove attachment
    const newAttachments = [
      ...attachmentsUploading.filter(
        a => a.identifier !== attachment.identifier
      )
    ];

    this.setState({
      attachmentsUploading: newAttachments
    });
  };

  private onUploadComplete = (
    response: AxiosResponse,
    attachment: UploadingInvoiceAttachment
  ) => {
    const { attachmentsUploading } = this.state;

    // find and remove attachment
    const newAttachments = [
      ...attachmentsUploading.filter(
        a => a.identifier !== attachment.identifier
      )
    ];

    this.setState({
      attachmentsUploading: newAttachments
    });

    // push completed attachment up
    this.props.onFileUpload({
      contentType: attachment.contentType,
      fileName: attachment.fileName,
      identifier: response.data.identifier,
      size: attachment.size
    });
  };
}
