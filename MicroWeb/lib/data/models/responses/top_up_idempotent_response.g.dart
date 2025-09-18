// GENERATED CODE - DO NOT MODIFY BY HAND

part of 'top_up_idempotent_response.dart';

// **************************************************************************
// JsonSerializableGenerator
// **************************************************************************

TopUpIdempotentResponse _$TopUpIdempotentResponseFromJson(
  Map<String, dynamic> json,
) => $checkedCreate('TopUpIdempotentResponse', json, ($checkedConvert) {
  final val = TopUpIdempotentResponse(
    status: $checkedConvert('status', (v) => v as String?),
    correlationId: $checkedConvert('correlationId', (v) => v as String?),
  );
  return val;
});

Map<String, dynamic> _$TopUpIdempotentResponseToJson(
  TopUpIdempotentResponse instance,
) => <String, dynamic>{
  'status': ?instance.status,
  'correlationId': ?instance.correlationId,
};
