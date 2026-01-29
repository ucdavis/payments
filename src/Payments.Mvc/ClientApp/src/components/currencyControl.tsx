import * as React from 'react';
import NumberControl from './numberControl';

interface IProps {
  value: number;
  onChange: (value: number) => void;
  isInvalid?: boolean;
  inputRef?: ((el: HTMLInputElement | null) => void) | React.RefObject<HTMLInputElement>;
  disabled?: boolean;
}

export default class CurrencyControl extends React.PureComponent<IProps, {}> {
  public render() {
    const { value, onChange, isInvalid, inputRef, disabled } = this.props;

    return (
      <NumberControl
        className={`text-end ${isInvalid ? 'is-invalid' : ''}`}
        min={0.01}
        step={0.01}
        placeholder='0.00'
        value={value}
        onChange={onChange}
        required={true}
        inputRef={inputRef}
        disabled={disabled}
      />
    );
  }
}
