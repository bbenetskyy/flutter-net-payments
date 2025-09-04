// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'account_response.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

AccountResponse _$AccountResponseFromJson(Map<String, dynamic> json) =>
    $checkedCreate('AccountResponse', json, ($checkedConvert) {
      final val = AccountResponse(
        id: $checkedConvert('id', (v) => v as String?),
        userId: $checkedConvert('userId', (v) => v as String?),
        iban: $checkedConvert('iban', (v) => v as String?),
        currency: $checkedConvert('currency', (v) => $enumDecodeNullable(_$CurrencyEnumMap, v)),
        createdAt: $checkedConvert('createdAt', (v) => v == null ? null : DateTime.parse(v as String)),
      );
      return val;
    });

Map<String, dynamic> _$AccountResponseToJson(AccountResponse instance) {
  final val = <String, dynamic>{};

  void writeNotNull(String key, dynamic value) {
    if (value != null) val[key] = value;
  }

  writeNotNull('id', instance.id);
  writeNotNull('userId', instance.userId);
  writeNotNull('iban', instance.iban);
  writeNotNull('currency', _$CurrencyEnumMap[instance.currency]);
  writeNotNull('createdAt', instance.createdAt?.toIso8601String());
  return val;
}

const _$CurrencyEnumMap = {Currency.EUR: 0, Currency.USD: 1, Currency.PLN: 2, Currency.GBP: 3};
