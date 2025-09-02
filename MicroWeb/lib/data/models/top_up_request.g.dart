// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'top_up_request.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

TopUpRequest _$TopUpRequestFromJson(Map<String, dynamic> json) =>
    $checkedCreate('TopUpRequest', json, ($checkedConvert) {
      final val = TopUpRequest(
        amountMinor: $checkedConvert(
          'amountMinor',
          (v) => (v as num?)?.toInt(),
        ),
        currency: $checkedConvert(
          'currency',
          (v) => $enumDecodeNullable(_$CurrencyEnumMap, v),
        ),
        correlationId: $checkedConvert('correlationId', (v) => v as String?),
        description: $checkedConvert('description', (v) => v as String?),
      );
      return val;
    });

Map<String, dynamic> _$TopUpRequestToJson(TopUpRequest instance) =>
    <String, dynamic>{
      'amountMinor': ?instance.amountMinor,
      'currency': ?_$CurrencyEnumMap[instance.currency],
      'correlationId': ?instance.correlationId,
      'description': ?instance.description,
    };

const _$CurrencyEnumMap = {
  Currency.EUR: 0,
  Currency.USD: 1,
  Currency.PLN: 2,
  Currency.GBP: 3,
};
