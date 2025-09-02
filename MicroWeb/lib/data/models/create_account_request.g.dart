// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'create_account_request.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

CreateAccountRequest _$CreateAccountRequestFromJson(
  Map<String, dynamic> json,
) => $checkedCreate('CreateAccountRequest', json, ($checkedConvert) {
  final val = CreateAccountRequest(
    iban: $checkedConvert('iban', (v) => v as String?),
    currency: $checkedConvert(
      'currency',
      (v) => $enumDecodeNullable(_$CurrencyEnumMap, v),
    ),
  );
  return val;
});

Map<String, dynamic> _$CreateAccountRequestToJson(
  CreateAccountRequest instance,
) => <String, dynamic>{
  'iban': ?instance.iban,
  'currency': ?_$CurrencyEnumMap[instance.currency],
};

const _$CurrencyEnumMap = {
  Currency.EUR: 0,
  Currency.USD: 1,
  Currency.PLN: 2,
  Currency.GBP: 3,
};
