// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'top_up_applied_response.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

TopUpAppliedResponse _$TopUpAppliedResponseFromJson(
  Map<String, dynamic> json,
) => $checkedCreate('TopUpAppliedResponse', json, ($checkedConvert) {
  final val = TopUpAppliedResponse(
    status: $checkedConvert('status', (v) => v as String?),
    correlationId: $checkedConvert('correlationId', (v) => v as String?),
    walletId: $checkedConvert('walletId', (v) => v as String?),
    userId: $checkedConvert('userId', (v) => v as String?),
    currency: $checkedConvert(
      'currency',
      (v) => $enumDecodeNullable(_$CurrencyEnumMap, v),
    ),
    balanceMinor: $checkedConvert('balanceMinor', (v) => (v as num?)?.toInt()),
  );
  return val;
});

Map<String, dynamic> _$TopUpAppliedResponseToJson(
  TopUpAppliedResponse instance,
) => <String, dynamic>{
  'status': ?instance.status,
  'correlationId': ?instance.correlationId,
  'walletId': ?instance.walletId,
  'userId': ?instance.userId,
  'currency': ?_$CurrencyEnumMap[instance.currency],
  'balanceMinor': ?instance.balanceMinor,
};

const _$CurrencyEnumMap = {
  Currency.EUR: 0,
  Currency.USD: 1,
  Currency.PLN: 2,
  Currency.GBP: 3,
};
