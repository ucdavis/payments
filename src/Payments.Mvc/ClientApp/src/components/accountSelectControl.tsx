import * as React from 'react';

import { Account } from '../models/Account';

import Modal from './modal';

interface IProps {
  accounts: Account[];
  value: number;
  onChange: (value: number) => void;
}

interface IState {
  isModalOpen: boolean;
}

function sortAccounts(a: Account, b: Account) {
  if (a.isDefault) {
    return -1;
  }

  if (b.isDefault) {
    return 1;
  }

  if (a.name < b.name) {
    return -1;
  }

  if (a.name > b.name) {
    return 1;
  }

  return 0;
}

export default class AccountSelectControl extends React.Component<
  IProps,
  IState
> {
  constructor(props: any) {
    super(props);

    this.state = {
      isModalOpen: false
    };
  }

  public render() {
    const { accounts, value, onChange } = this.props;

    const ordered = [...accounts].sort(sortAccounts);

    return (
      <div className='input-group'>
        <select
          className='form-control'
          value={value}
          onChange={e => onChange(Number(e.target.value))}
          required={true}
        >
          {ordered.map(a => (
            <option key={a.id} value={a.id}>
              {a.name}
            </option>
          ))}
        </select>
        <div className='input-group-text'>
          <button className='btn' type='button' onClick={this.openModal}>
            <i className='fas fa-fw fa-info-circle' />
          </button>
        </div>
        {this.renderModal()}
      </div>
    );
  }

  private renderModal() {
    const { accounts } = this.props;
    const { isModalOpen } = this.state;

    const ordered = [...accounts].sort(sortAccounts);

    return (
      <Modal
        dialogClassName='account-details-modal modal-lg'
        isOpen={isModalOpen}
        onBackdropClick={this.closeModal}
        onEscape={this.closeModal}
      >
        <div className='modal-header'>
          <div className='row flex-grow-1'>
            <div className='col-md-3' />
            <div className='col-md-6 d-flex justify-content-center align-items-center'>
              <span className='modal-title'>Accounts Details</span>
            </div>
            <div className='col-md-3 d-flex justify-content-end align-items-center'>
              <button
                type='button'
                className='close m-1'
                onClick={this.closeModal}
              >
                <span aria-hidden='true'>
                  <i className='fas fa-times' />
                </span>
              </button>
            </div>
          </div>
        </div>
        {ordered.map(this.renderAccountDetails)}
      </Modal>
    );
  }

  private renderAccountDetails = (item: Account) => {
    const selectedValue = this.props.value;
    const {
      id,
      name,
      description,
      isDefault,
      chart,
      account,
      subAccount,
      object,
      subObject,
      project,
      financialSegmentString
    } = item;

    let accountString = `${chart}-${account}`;
    if (subAccount) {
      accountString += `-${subAccount}`;
    }

    let objectString = object;
    if (subObject) {
      objectString += `-${subObject}`;
    }

    return (
      <div className='modal-body' key={id}>
        <div className='row flex-grow-1'>
          <div className='col-md-4'>
            <span className='account-name'>{name}</span>
            <p className='account-description'>{description}</p>
          </div>
          {chart && (
            <div className='col-md-4 d-flex flex-column'>
              <dl className='row'>
                <dt className='col-sm-4'>Account</dt>
                <dd className='col-sm-8'>{accountString}</dd>

                <dt className='col-sm-4'>Object</dt>
                <dd className='col-sm-8'>{objectString}</dd>

                <dt className='col-sm-4'>Project</dt>
                <dd className='col-sm-8'>{project}</dd>
              </dl>
            </div>
          )}
          {!chart && <div className='col-md-4 d-flex flex-column'> </div>}
          <div className='col-md-3 d-flex flex-column justify-content-around align-items-center'>
            {isDefault && <span className='badge text-bg-info'>default</span>}
            {selectedValue === id && (
              <span className='badge text-bg-success'>selected</span>
            )}
          </div>
          <div className='col-md-1 d-flex justify-content-end align-items-center'>
            <button className='btn' onClick={() => this.modalPickAccount(id)}>
              <i className='fas fa-angle-right' />
            </button>
          </div>

          {financialSegmentString && (
            <div className='col-md-8 '>
              {' '}
              <p className='small'> {financialSegmentString} </p>
            </div>
          )}
        </div>
      </div>
    );
  };

  private openModal = () => {
    this.setState({ isModalOpen: true });
  };

  private closeModal = () => {
    this.setState({ isModalOpen: false });
  };

  private modalPickAccount = (id: number) => {
    this.props.onChange(id);
    this.closeModal();
  };
}
