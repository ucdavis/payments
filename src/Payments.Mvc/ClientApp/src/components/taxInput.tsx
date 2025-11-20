import * as React from 'react';
import NumberControl from './numberControl';

interface IProps {
  value: number;
  onChange: (value: number) => void;
}

interface IState {
  hasTax: boolean;
}

export default class TaxInput extends React.Component<IProps, IState> {
  constructor(props: IProps) {
    super(props);

    this.state = {
      hasTax: !!props.value
    };
  }

  public render() {
    const { onChange, value } = this.props;

    if (!this.state.hasTax) {
      return (
        <button
          className='btn btn-outline-primary btn-sm'
          onClick={this.addTax}
        >
          <i className='fas fa-plus me-2' /> Add tax
        </button>
      );
    }

    return (
      <div className='input-group'>
        <NumberControl
          min={0.001}
          step={0.0001}
          placeholder={'0.000'}
          value={value}
          onChange={onChange}
          format={this.renderTax}
          required={true}
        />
        <div className='input-group-text'>%</div>
        <div className='invalid-feedback'>Set a tax or remove.</div>
      </div>
    );
  }

  private addTax = () => {
    this.setState({ hasTax: true });
  };

  private renderTax = (value: number) => {
    if (value === 0) {
      return '0.000';
    }

    return value.toFixed(4);
  };
}
