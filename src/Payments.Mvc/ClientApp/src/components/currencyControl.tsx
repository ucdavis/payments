import * as React from 'react';
import NumberControl from './numberControl';

interface IProps {
  value: number;
  onChange: (value: number) => void;
  isInvalid?: boolean;
  inputRef?: React.RefObject<HTMLInputElement>;
}

export default class CurrencyControl extends React.PureComponent<IProps, {}> {
  public render() {
    const { value, onChange, isInvalid, inputRef } = this.props;

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
      />
    );
  }
}
