// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'create_card_request.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

CreateCardRequest _$CreateCardRequestFromJson(Map<String, dynamic> json) =>
    $checkedCreate('CreateCardRequest', json, ($checkedConvert) {
      final val = CreateCardRequest(
        type: $checkedConvert(
          'type',
          (v) => $enumDecodeNullable(_$CardTypeEnumMap, v),
        ),
        name: $checkedConvert('name', (v) => v as String?),
        singleTransactionLimit: $checkedConvert(
          'singleTransactionLimit',
          (v) => (v as num?)?.toInt(),
        ),
        monthlyLimit: $checkedConvert(
          'monthlyLimit',
          (v) => (v as num?)?.toInt(),
        ),
      );
      return val;
    });

Map<String, dynamic> _$CreateCardRequestToJson(CreateCardRequest instance) =>
    <String, dynamic>{
      'type': ?_$CardTypeEnumMap[instance.type],
      'name': ?instance.name,
      'singleTransactionLimit': ?instance.singleTransactionLimit,
      'monthlyLimit': ?instance.monthlyLimit,
    };

const _$CardTypeEnumMap = {CardType.Personal: 0, CardType.Shared: 1};
