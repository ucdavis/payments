import * as React from 'react';

interface IProps {
  className: string;
  onDismiss: () => void;
  children?: React.ReactNode;
}

export default class Alert extends React.Component<IProps, {}> {
  public render() {
    const { className, onDismiss } = this.props;

    return (
      <div
        className={
          'd-flex justify-content-between align-items-center alert alert-dismissible ' +
          className
        }
        role='alert'
      >
        <div>{this.props.children}</div>
        <button
          type='button'
          className='btn-close'
          aria-label='Close'
          onClick={onDismiss}
        ></button>
      </div>
    );
  }
}
